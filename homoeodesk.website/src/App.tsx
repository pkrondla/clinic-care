const DEMO_TENANT_URL = import.meta.env.VITE_DEMO_TENANT_URL || 'http://localhost:3000'
const ADMIN_PORTAL_URL = import.meta.env.VITE_ADMIN_PORTAL_URL || 'http://localhost:3100'

export default function App() {
  return (
    <div className="page">
      <header className="header">
        <div className="brand">HomoeoDesk</div>
        <nav className="nav">
          <a href="#features">Features</a>
          <a href={DEMO_TENANT_URL}>Demo</a>
          <a href={ADMIN_PORTAL_URL} className="nav-cta">Admin Portal</a>
        </nav>
      </header>

      <main>
        <section className="hero">
          <p className="eyebrow">homoeodesk.com</p>
          <h1>Modern clinic management for homoeopathy practices</h1>
          <p className="lead">
            Appointments, prescriptions, inventory, billing, and patient records —
            everything your clinic team needs in one place.
          </p>
          <div className="hero-actions">
            <a className="button primary" href={DEMO_TENANT_URL}>
              Try Demo Tenant
            </a>
            <a className="button secondary" href={ADMIN_PORTAL_URL}>
              Global Admin Portal
            </a>
          </div>
        </section>

        <section id="features" className="features">
          <article>
            <h2>Patient care</h2>
            <p>Consultations, prescriptions, and follow-ups with a workflow built for homoeopaths.</p>
          </article>
          <article>
            <h2>Operations</h2>
            <p>Queue management, appointments, inventory, and purchase orders for your front desk.</p>
          </article>
          <article>
            <h2>Insights</h2>
            <p>Reports on collections, inventory, and patient activity to run your practice with clarity.</p>
          </article>
        </section>
      </main>

      <footer className="footer">
        <span>© {new Date().getFullYear()} HomoeoDesk</span>
        <span>
          <a href={DEMO_TENANT_URL}>Demo tenant</a>
          {' · '}
          <a href={ADMIN_PORTAL_URL}>Admin portal</a>
        </span>
      </footer>
    </div>
  )
}
