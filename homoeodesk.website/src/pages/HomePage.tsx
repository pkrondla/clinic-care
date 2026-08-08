import { Link } from 'react-router-dom'
import { Seo } from '../components/Seo'
import { CtaBand } from '../components/CtaBand'
import { ProductVisual } from '../components/ProductVisual'
import { SectionHeading } from '../components/SectionHeading'
import { DEMO_TENANT_URL } from '../config'
import { trackEvent } from '../analytics'
import { heroCopy, pillars, teamRoles, workflowSteps } from '../content/site'

export function HomePage() {
  return (
    <>
      <Seo
        title="HomoeoDesk — Homoeopathy Clinic Management"
        description="Modern clinic management for homoeopathy practices. Appointments, prescriptions, inventory, billing, and patient records in one place."
        path="/"
      />

      <section className="hero">
        <div className="hero-bg" />
        <div className="container hero-inner">
          <div className="hero-brand">{heroCopy.brand}</div>
          <h1>{heroCopy.headline}</h1>
          <p className="lead">{heroCopy.lead}</p>
          <div className="hero-actions">
            <Link
              to="/register"
              className="button primary"
              onClick={() => trackEvent('hero_start_trial')}
            >
              {heroCopy.primaryCta}
            </Link>
            <a
              className="button secondary"
              href={DEMO_TENANT_URL}
              target="_blank"
              rel="noreferrer"
              onClick={() => trackEvent('hero_try_demo')}
            >
              {heroCopy.secondaryCta}
            </a>
          </div>
        </div>
      </section>

      <section className="section">
        <div className="container">
          <SectionHeading
            title="How clinics use HomoeoDesk"
            lead="One continuous day — from the front desk to the consult to dispensing and payment."
          />
          <div className="workflow">
            {workflowSteps.map((step, index) => (
              <article key={step.title}>
                <div className="step">Step {index + 1}</div>
                <h3>{step.title}</h3>
                <p>{step.body}</p>
              </article>
            ))}
          </div>
          <ProductVisual />
        </div>
      </section>

      <section className="section" style={{ paddingTop: 0 }}>
        <div className="container">
          <SectionHeading
            title="Built for homoeopathy — not a generic EMR"
            lead="The product language matches your remedies, queues, and clinic roles."
          />
          <div className="pillars">
            {pillars.map((pillar) => (
              <article className="pillar" key={pillar.title}>
                <h3>{pillar.title}</h3>
                <p>{pillar.body}</p>
              </article>
            ))}
          </div>
        </div>
      </section>

      <section className="section" style={{ paddingTop: 0 }}>
        <div className="container">
          <SectionHeading
            title="For the whole clinic team"
            lead="Everyone works in the same system with the right level of access."
          />
          <div className="roles">
            {teamRoles.map((item) => (
              <article className="role-card" key={item.role}>
                <h3>{item.role}</h3>
                <p>{item.body}</p>
              </article>
            ))}
          </div>
        </div>
      </section>

      <CtaBand />
    </>
  )
}
