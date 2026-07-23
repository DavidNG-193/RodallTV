export function DashboardPage() {
  return (
    <section>
      <div className="page-heading">
        <div>
          <p className="page-heading__eyebrow">Resumen</p>
          <h2>Panel principal</h2>
          <p>Consulta el estado general del sistema de Digital Signage.</p>
        </div>
      </div>

      <div className="summary-grid">
        <article className="summary-card">
          <span>Dispositivos</span>
          <strong>—</strong>
          <small>El resumen se conectará más adelante.</small>
        </article>
        <article className="summary-card">
          <span>Contenido multimedia</span>
          <strong>—</strong>
          <small>Información pendiente de integrar.</small>
        </article>
        <article className="summary-card">
          <span>Playlists</span>
          <strong>—</strong>
          <small>Información pendiente de integrar.</small>
        </article>
      </div>
    </section>
  );
}