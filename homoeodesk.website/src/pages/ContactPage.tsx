import { Link, useSearchParams } from 'react-router-dom'
import { Seo } from '../components/Seo'
import { BOOKING_URL, CONTACT_EMAIL } from '../config'

export function ContactPage() {
  const [params] = useSearchParams()
  const plan = params.get('plan')
  const subject = plan ? `HomoeoDesk ${plan} plan inquiry` : 'HomoeoDesk inquiry'

  return (
    <>
      <Seo
        title="Contact"
        description="Contact HomoeoDesk sales and support for trials, pricing, and onboarding."
        path="/contact"
      />
      <section className="page-hero">
        <div className="container">
          <h1>Contact us</h1>
          <p>Questions about pricing, onboarding, or whether HomoeoDesk fits your clinic? We are here.</p>
        </div>
      </section>
      <section className="container split-two">
        <article className="panel">
          <h2 style={{ fontSize: '1.3rem', marginBottom: '0.75rem' }}>Email</h2>
          <p style={{ color: 'var(--ink-soft)', marginBottom: '1rem' }}>
            Write to us and include your clinic name, city, and approximate weekly patient volume.
          </p>
          <a className="button primary" href={`mailto:${CONTACT_EMAIL}?subject=${encodeURIComponent(subject)}`}>
            {CONTACT_EMAIL}
          </a>
        </article>
        <article className="panel">
          <h2 style={{ fontSize: '1.3rem', marginBottom: '0.75rem' }}>Faster paths</h2>
          <ul style={{ margin: 0, paddingLeft: '1.1rem', color: 'var(--ink-soft)' }}>
            <li>
              <Link to="/register">Start a self-serve trial registration</Link>
            </li>
            <li>
              <Link to="/request-trial">Request a guided trial</Link>
            </li>
            <li>
              <Link to="/demo">Open the demo clinic</Link>
            </li>
            {BOOKING_URL ? (
              <li>
                <a href={BOOKING_URL} target="_blank" rel="noreferrer">
                  Book a call
                </a>
              </li>
            ) : null}
          </ul>
        </article>
      </section>
    </>
  )
}
