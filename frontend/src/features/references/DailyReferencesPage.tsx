import { RefreshCw, Trash2 } from "lucide-react";
import { useCallback, useEffect, useRef, useState } from "react";
import { EmptyState } from "../../components/common/EmptyState";
import { LoadingState } from "../../components/common/LoadingState";
import { ConfirmationDialog } from "./components/ConfirmationDialog";
import { DailyReferencesTable } from "./components/DailyReferencesTable";
import { ReferenceLookupForm } from "./components/ReferenceLookupForm";
import { ReferencesSummary } from "./components/ReferencesSummary";
import { getReferenceErrorMessage } from "./referenceErrors";
import { referencesApi } from "./referencesApi";
import type { DailyReference } from "./types";
import "./DailyReferencesPage.css";

const pollingIntervalMilliseconds = 30_000;

type ConfirmationTarget =
  | { kind: "single"; reference: DailyReference }
  | { kind: "all" };

export function DailyReferencesPage() {
  const [references, setReferences] = useState<DailyReference[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [isDeletingAll, setIsDeletingAll] = useState(false);
  const [loadError, setLoadError] = useState("");
  const [actionError, setActionError] = useState("");
  const [confirmation, setConfirmation] = useState<ConfirmationTarget | null>(null);
  const listRequestInFlight = useRef(false);

  const loadReferences = useCallback(async (showLoading = false) => {
    if (listRequestInFlight.current) return;

    listRequestInFlight.current = true;
    if (showLoading) setIsLoading(true);

    try {
      setReferences(await referencesApi.getDailyReferences());
      setLoadError("");
    } catch (error) {
      setLoadError(getReferenceErrorMessage(error, "load"));
    } finally {
      listRequestInFlight.current = false;
      if (showLoading) setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    const initialLoadId = window.setTimeout(() => {
      void loadReferences(true);
    }, 0);

    const intervalId = window.setInterval(() => {
      void loadReferences();
    }, pollingIntervalMilliseconds);

    return () => {
      window.clearTimeout(initialLoadId);
      window.clearInterval(intervalId);
    };
  }, [loadReferences]);

  const handleCreated = (created: DailyReference) => {
    setActionError("");
    setReferences((current) => [
      created,
      ...current.filter((reference) => reference.id !== created.id),
    ]);
  };

  const handleRefresh = async () => {
    if (isRefreshing) return;

    setIsRefreshing(true);
    setActionError("");

    try {
      await referencesApi.refreshDailyReferences();
      setReferences(await referencesApi.getDailyReferences());
      setLoadError("");
    } catch (error) {
      setActionError(getReferenceErrorMessage(error, "refresh"));
    } finally {
      setIsRefreshing(false);
    }
  };

  const closeConfirmation = useCallback(() => {
    if (!deletingId && !isDeletingAll) setConfirmation(null);
  }, [deletingId, isDeletingAll]);

  const confirmDelete = async () => {
    if (!confirmation) return;

    setActionError("");

    if (confirmation.kind === "all") {
      setIsDeletingAll(true);

      try {
        await referencesApi.deleteAllDailyReferences();
        setReferences([]);
        setConfirmation(null);
      } catch (error) {
        setActionError(getReferenceErrorMessage(error, "delete"));
      } finally {
        setIsDeletingAll(false);
      }

      return;
    }

    const reference = confirmation.reference;
    setDeletingId(reference.id);

    try {
      await referencesApi.deleteDailyReference(reference.id);
      setReferences((current) => current.filter((item) => item.id !== reference.id));
      setConfirmation(null);
    } catch (error) {
      setActionError(getReferenceErrorMessage(error, "delete"));
    } finally {
      setDeletingId(null);
    }
  };

  const confirmationContent = confirmation?.kind === "single"
    ? {
        title: "Eliminar referencia",
        message: `¿Deseas eliminar la referencia ${confirmation.reference.referenceNumber} de las pantallas?`,
        confirmLabel: "Eliminar referencia",
      }
    : {
        title: "Eliminar todas las referencias",
        message: "Se eliminarán todas las referencias actualmente mostradas en las pantallas. Esta acción no puede deshacerse en RodallTV.",
        confirmLabel: "Eliminar todas",
      };

  return (
    <section className="references-page">
      <div className="page-heading references-page__heading">
        <div>
          <p className="page-heading__eyebrow">Panel de señalización</p>
          <h2>Referencias del día</h2>
          <p>Consulta en Saga y administra las referencias visibles en todas las pantallas.</p>
        </div>
      </div>

      <ReferenceLookupForm onCreated={handleCreated} />

      {(loadError || actionError) && (
        <div className="alert alert--error references-page__alert" role="alert">
          <span>{actionError || loadError}</span>
          {loadError && (
            <button
              type="button"
              className="button button--secondary"
              onClick={() => void loadReferences(true)}
            >
              Reintentar
            </button>
          )}
        </div>
      )}

      <section className="references-card references-list-card">
        <div className="references-list-card__header">
          <div>
            <span className="references-list-card__eyebrow">
              Referencias activas en pantalla
            </span>
            <ReferencesSummary references={references} />
          </div>

          <div className="references-list-card__actions">
            <button
              type="button"
              className="button button--secondary"
              disabled={isRefreshing}
              onClick={() => void handleRefresh()}
            >
              <RefreshCw
                size={18}
                aria-hidden="true"
                className={isRefreshing ? "references-spin" : undefined}
              />
              {isRefreshing ? "Actualizando..." : "Actualizar"}
            </button>

            <button
              type="button"
              className="button references-button--danger-quiet"
              disabled={references.length === 0 || isDeletingAll}
              onClick={() => setConfirmation({ kind: "all" })}
            >
              <Trash2 size={18} aria-hidden="true" />
              Eliminar todas
            </button>
          </div>
        </div>

        {isLoading ? (
          <LoadingState message="Cargando referencias..." />
        ) : references.length === 0 ? (
          <EmptyState
            title="No hay referencias activas en pantalla"
            description="Busca una referencia y agrégala para comenzar a mostrarla."
          />
        ) : (
          <DailyReferencesTable
            references={references}
            deletingId={deletingId}
            onDelete={(reference) => setConfirmation({ kind: "single", reference })}
          />
        )}
      </section>

      {confirmation && (
        <ConfirmationDialog
          {...confirmationContent}
          isBusy={Boolean(deletingId) || isDeletingAll}
          onCancel={closeConfirmation}
          onConfirm={() => void confirmDelete()}
        />
      )}
    </section>
  );
}
