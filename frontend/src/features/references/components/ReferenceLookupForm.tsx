import { Search } from "lucide-react";
import { useState, type ChangeEvent, type FormEvent } from "react";
import { getReferenceErrorMessage } from "../referenceErrors";
import { referencesApi } from "../referencesApi";
import type { DailyReference, ReferenceLookup } from "../types";
import { ReferencePreview } from "./ReferencePreview";

interface ReferenceLookupFormProps {
  onCreated: (reference: DailyReference) => void;
}

export function ReferenceLookupForm({
  onCreated,
}: ReferenceLookupFormProps) {
  const [referenceNumber, setReferenceNumber] = useState("");
  const [lookupResult, setLookupResult] = useState<ReferenceLookup | null>(null);
  const [isSearching, setIsSearching] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const [error, setError] = useState("");

  const handleReferenceChange = (event: ChangeEvent<HTMLInputElement>) => {
    setReferenceNumber(event.target.value.toUpperCase());
    setLookupResult(null);
    setError("");
  };

  const handleLookup = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const normalized = referenceNumber.trim().toUpperCase();
    if (!normalized) {
      setError("Escribe un número de referencia.");
      return;
    }

    setReferenceNumber(normalized);
    setIsSearching(true);
    setError("");

    try {
      setLookupResult(await referencesApi.lookupReference(normalized));
    } catch (requestError) {
      setLookupResult(null);
      setError(getReferenceErrorMessage(requestError, "lookup"));
    } finally {
      setIsSearching(false);
    }
  };

  const handleCreate = async () => {
    if (!lookupResult || isCreating) return;

    setIsCreating(true);
    setError("");

    try {
      const created = await referencesApi.createDailyReference(
        lookupResult.referenceNumber,
      );
      onCreated(created);
      setReferenceNumber("");
      setLookupResult(null);
    } catch (requestError) {
      setError(getReferenceErrorMessage(requestError, "create"));
    } finally {
      setIsCreating(false);
    }
  };

  return (
    <section className="references-card references-lookup-card">
      <div className="references-card__heading">
        <div>
          <span>Nueva referencia</span>
          <h3>Consulta antes de agregar</h3>
        </div>
        <p>Los datos mostrados se obtienen directamente desde Saga.</p>
      </div>

      <form className="reference-search" onSubmit={(event) => void handleLookup(event)}>
        <div className="form-field reference-search__field">
          <label htmlFor="reference-number">Número de referencia</label>
          <input
            id="reference-number"
            name="referenceNumber"
            type="text"
            value={referenceNumber}
            maxLength={50}
            autoComplete="off"
            placeholder="Ej. VER23-05198"
            onChange={handleReferenceChange}
          />
        </div>

        <button
          type="submit"
          className="button button--secondary reference-search__button"
          disabled={isSearching || isCreating || !referenceNumber.trim()}
        >
          <Search size={18} aria-hidden="true" />
          {isSearching ? "Buscando..." : "Buscar"}
        </button>
      </form>

      {error && (
        <div className="references-inline-error" role="alert">
          {error}
        </div>
      )}

      {lookupResult && (
        <ReferencePreview
          reference={lookupResult}
          isCreating={isCreating}
          onCreate={() => void handleCreate()}
        />
      )}
    </section>
  );
}
