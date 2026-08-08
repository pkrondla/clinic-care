import { Link } from 'react-router-dom'
import { Seo } from '../components/Seo'
import { CtaBand } from '../components/CtaBand'
import { plans, pricingFaqs } from '../content/pricing'
import { trackEvent } from '../analytics'

export function PricingPage() {
  return (
    <>
      <Seo
        title="Pricing"
        description="HomoeoDesk pricing: 30-day free trial, Clinic, and Practice+ plans for homoeopathy clinics."
        path="/pricing"
      />
      <section className="page-hero">
        <div className="container">
          <h1>Simple plans for growing clinics</h1>
          <p>
            Start with a 30-day trial. Paid plans are tailored to your branch count and team size —
            tell us about your practice and we will recommend the right fit.
          </p>
        </div>
      </section>
      <section className="container plans" style={{ paddingBottom: '2rem' }}>
        {plans.map((plan) => (
          <article className={`plan${plan.highlighted ? ' highlighted' : ''}`} key={plan.id}>
            <h3>{plan.name}</h3>
            <div className="price">{plan.price}</div>
            <div className="period">{plan.period}</div>
            <p style={{ color: 'var(--ink-soft)', marginBottom: '1rem' }}>{plan.description}</p>
            <ul>
              {plan.features.map((feature) => (
                <li key={feature}>{feature}</li>
              ))}
            </ul>
            <Link
              to={plan.ctaTo}
              className={`button ${plan.highlighted ? 'primary' : 'secondary'}`}
              onClick={() => trackEvent('pricing_cta', { plan: plan.id })}
            >
              {plan.cta}
            </Link>
          </article>
        ))}
      </section>
      <section className="container">
        <h2 style={{ fontSize: '1.6rem', marginBottom: '1rem' }}>FAQ</h2>
        <div className="faq">
          {pricingFaqs.map((item) => (
            <details key={item.q}>
              <summary>{item.q}</summary>
              <p>{item.a}</p>
            </details>
          ))}
        </div>
      </section>
      <CtaBand />
    </>
  )
}
