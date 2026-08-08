import { Link } from 'react-router-dom'
import { Seo } from '../components/Seo'

export function SecurityPage() {
  return (
    <>
      <Seo
        title="Security"
        description="How HomoeoDesk approaches tenant isolation, access control, and clinic data protection."
        path="/security"
      />
      <section className="page-hero">
        <div className="container">
          <h1>Security & data stewardship</h1>
          <p>
            Clinic data is sensitive. HomoeoDesk is built as a multi-tenant platform with organization
            isolation and role-based access inside each clinic.
          </p>
        </div>
      </section>
      <section className="container prose">
        <h2>Tenant isolation</h2>
        <p>
          Each organization is registered in the global control plane and runs against its own tenant
          database. Clinic users authenticate to their tenant application — not the platform admin portal.
        </p>
        <h2>Access control</h2>
        <p>
          Tenant roles (Admin, Doctor, Staff, Patient) limit what each person can see and do. Platform
          SuperAdmin access is reserved for HomoeoDesk operators and is separate from clinic logins.
        </p>
        <h2>Transport & hosting</h2>
        <p>
          Production deployments use HTTPS and Azure-hosted infrastructure. Secrets can be loaded from
          Azure Key Vault; application telemetry may use Application Insights for operational health.
        </p>
        <h2>Your responsibilities</h2>
        <ul>
          <li>Do not enter real patient data into the shared demo clinic</li>
          <li>Use strong passwords and limit admin accounts to trusted staff</li>
          <li>Configure notification providers carefully (WhatsApp, SMS, email)</li>
        </ul>
        <h2>Questions</h2>
        <p>
          For security inquiries, contact us via the <Link to="/contact">Contact</Link> page.
        </p>
      </section>
    </>
  )
}
