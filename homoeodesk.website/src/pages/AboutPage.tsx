import { Seo } from '../components/Seo'
import { CtaBand } from '../components/CtaBand'

export function AboutPage() {
  return (
    <>
      <Seo
        title="About"
        description="HomoeoDesk is multi-tenant clinic software purpose-built for homoeopathy practices."
        path="/about"
      />
      <section className="page-hero">
        <div className="container">
          <h1>About HomoeoDesk</h1>
          <p>
            HomoeoDesk is clinic management software for homoeopathy practices — designed around
            consultations, remedy prescriptions, front-desk flow, inventory, and billing.
          </p>
        </div>
      </section>
      <section className="container prose">
        <h2>Why we built it</h2>
        <p>
          Generic EMR and clinic tools force homoeopaths into forms that do not match how cases are
          taken or how remedies are prescribed. HomoeoDesk starts from that clinical reality and wraps
          the rest of the clinic around it.
        </p>
        <h2>How we deliver</h2>
        <p>
          HomoeoDesk is a multi-tenant SaaS. Each clinic organization gets an isolated tenant
          environment, while a shared platform manages subscriptions, the global medicine catalog, and
          onboarding.
        </p>
        <h2>Who we serve</h2>
        <p>
          Independent homoeopathy clinics and multi-branch practices that want one system for doctors,
          reception, and inventory — without bolting together spreadsheets and generic tools.
        </p>
      </section>
      <CtaBand />
    </>
  )
}
