import { Link } from 'react-router-dom'
import { Seo } from '../components/Seo'
import { DEMO_TENANT_URL } from '../config'
import { trackEvent } from '../analytics'
import { demoAccounts } from '../content/site'

export function DemoPage() {
  return (
    <>
      <Seo
        title="Demo clinic"
        description="Try the HomoeoDesk shared demo clinic with sample admin, doctor, and reception accounts."
        path="/demo"
      />
      <section className="page-hero">
        <div className="container">
          <h1>Try the demo clinic</h1>
          <p>
            Explore a shared sandbox with sample patients, appointments, and inventory. It is great for
            a first look — for your own data and team, start a trial instead.
          </p>
        </div>
      </section>
      <section className="container demo-grid">
        <article className="panel">
          <h2 style={{ fontSize: '1.35rem', marginBottom: '0.75rem' }}>What to expect</h2>
          <ul style={{ margin: 0, paddingLeft: '1.1rem', color: 'var(--ink-soft)' }}>
            <li>Shared demo data — please do not enter real patient information</li>
            <li>Roles for Admin, Doctor, and Reception so you can walk the full workflow</li>
            <li>Features may reset periodically as the sandbox is refreshed</li>
            <li>Need your own environment? Start a free trial and we will provision your clinic</li>
          </ul>
          <div className="hero-actions" style={{ marginTop: '1.5rem' }}>
            <a
              className="button primary"
              href={DEMO_TENANT_URL}
              target="_blank"
              rel="noreferrer"
              onClick={() => trackEvent('demo_open_click')}
            >
              Open demo clinic
            </a>
            <Link to="/register" className="button secondary" onClick={() => trackEvent('demo_to_trial')}>
              Start free trial
            </Link>
          </div>
        </article>
        <article className="panel">
          <h2 style={{ fontSize: '1.35rem', marginBottom: '0.75rem' }}>Sample sign-in</h2>
          <p style={{ color: 'var(--muted)', marginBottom: '0.75rem' }}>
            Use the accounts below in the demo tenant. Password details are shown on the demo login screen
            when published for public use.
          </p>
          <div className="account-list">
            {demoAccounts.map((account) => (
              <div className="account" key={account.email}>
                <div>
                  <strong>{account.role}</strong>
                  <div style={{ color: 'var(--muted)', fontSize: '0.9rem' }}>{account.note}</div>
                </div>
                <code>{account.email}</code>
              </div>
            ))}
          </div>
        </article>
      </section>
    </>
  )
}
