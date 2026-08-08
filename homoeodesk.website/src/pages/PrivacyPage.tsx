import { Seo } from '../components/Seo'
import { CONTACT_EMAIL } from '../config'

export function PrivacyPage() {
  return (
    <>
      <Seo
        title="Privacy Policy"
        description="HomoeoDesk privacy policy for website visitors and clinic customers."
        path="/privacy"
      />
      <section className="page-hero">
        <div className="container">
          <h1>Privacy Policy</h1>
          <p>Last updated: 8 August 2026</p>
        </div>
      </section>
      <section className="container prose">
        <p>
          This policy describes how HomoeoDesk collects and uses information when you visit
          homoeodesk.com or use HomoeoDesk services.
        </p>
        <h2>Information we collect</h2>
        <ul>
          <li>Contact and clinic details you submit via trial, register, or contact forms</li>
          <li>Usage analytics if analytics is enabled for the website</li>
          <li>Clinic operational data you enter into a provisioned HomoeoDesk tenant</li>
        </ul>
        <h2>How we use information</h2>
        <ul>
          <li>To respond to trial and sales inquiries</li>
          <li>To create and operate your organization trial or subscription</li>
          <li>To improve the product and website experience</li>
        </ul>
        <h2>Sharing</h2>
        <p>
          We do not sell personal information. We may use infrastructure and communication providers
          (hosting, email, SMS, WhatsApp) strictly to deliver the service.
        </p>
        <h2>Retention</h2>
        <p>
          Lead and account information is retained as needed to provide the service and meet legal
          obligations. You may request deletion of marketing leads by emailing {CONTACT_EMAIL}.
        </p>
        <h2>Contact</h2>
        <p>
          Privacy questions: <a href={`mailto:${CONTACT_EMAIL}`}>{CONTACT_EMAIL}</a>
        </p>
      </section>
    </>
  )
}
