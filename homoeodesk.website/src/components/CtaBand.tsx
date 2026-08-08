import { Link } from 'react-router-dom'
import { DEMO_TENANT_URL } from '../config'
import { trackEvent } from '../analytics'

type CtaBandProps = {
  title?: string
  body?: string
}

export function CtaBand({
  title = 'Ready to run your clinic on HomoeoDesk?',
  body = 'Start a 30-day trial for your organization, or explore the shared demo clinic first.',
}: CtaBandProps) {
  return (
    <div className="container">
      <section className="cta-band">
        <div>
          <h2>{title}</h2>
          <p>{body}</p>
        </div>
        <div className="hero-actions">
          <Link
            to="/register"
            className="button primary"
            onClick={() => trackEvent('cta_start_trial')}
          >
            Start free trial
          </Link>
          <a
            className="button secondary"
            href={DEMO_TENANT_URL}
            target="_blank"
            rel="noreferrer"
            onClick={() => trackEvent('cta_try_demo')}
          >
            Try demo clinic
          </a>
        </div>
      </section>
    </div>
  )
}
