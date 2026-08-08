import { Seo } from '../components/Seo'
import { CONTACT_EMAIL } from '../config'

export function TermsPage() {
  return (
    <>
      <Seo
        title="Terms of Service"
        description="HomoeoDesk terms of service for website use and clinic subscriptions."
        path="/terms"
      />
      <section className="page-hero">
        <div className="container">
          <h1>Terms of Service</h1>
          <p>Last updated: 8 August 2026</p>
        </div>
      </section>
      <section className="container prose">
        <p>
          By using the HomoeoDesk website or registering for a trial, you agree to these terms. A
          separate customer agreement may apply once a paid subscription is activated.
        </p>
        <h2>Service</h2>
        <p>
          HomoeoDesk provides multi-tenant clinic management software. Trial registration creates an
          organization record; dedicated clinic access is provisioned as part of onboarding.
        </p>
        <h2>Acceptable use</h2>
        <ul>
          <li>Do not misuse the shared demo or attempt to disrupt the service</li>
          <li>Do not upload unlawful content or violate patient confidentiality obligations</li>
          <li>Keep login credentials confidential</li>
        </ul>
        <h2>Trials</h2>
        <p>
          Trials are time-limited (typically 30 days) and provided as-is. Features may change during
          early access.
        </p>
        <h2>Disclaimer</h2>
        <p>
          HomoeoDesk is clinic operations software and does not provide medical advice. Clinical
          decisions remain the responsibility of licensed practitioners.
        </p>
        <h2>Contact</h2>
        <p>
          Questions: <a href={`mailto:${CONTACT_EMAIL}`}>{CONTACT_EMAIL}</a>
        </p>
      </section>
    </>
  )
}
