import { Seo } from '../components/Seo'
import { CtaBand } from '../components/CtaBand'
import { featureGroups } from '../content/features'

export function FeaturesPage() {
  return (
    <>
      <Seo
        title="Features"
        description="Explore HomoeoDesk features for patient care, front desk operations, pharmacy, billing, and notifications."
        path="/features"
      />
      <section className="page-hero">
        <div className="container">
          <h1>Everything your clinic team needs</h1>
          <p>
            HomoeoDesk covers the full clinic day — consultations and prescriptions, appointments and
            queues, inventory and dispensing, billing and reports.
          </p>
        </div>
      </section>
      <section className="container feature-stack">
        {featureGroups.map((group) => (
          <article className="feature-block" id={group.id} key={group.id}>
            <h2>{group.title}</h2>
            <p className="lead">{group.lead}</p>
            <ul>
              {group.items.map((item) => (
                <li key={item}>{item}</li>
              ))}
            </ul>
          </article>
        ))}
      </section>
      <CtaBand title="See a feature in context" body="Open the demo clinic or start a trial for your practice." />
    </>
  )
}
