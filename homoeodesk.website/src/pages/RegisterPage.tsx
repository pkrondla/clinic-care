import { FormEvent, useState } from 'react'
import { Link } from 'react-router-dom'
import { Seo } from '../components/Seo'
import { DEMO_TENANT_URL } from '../config'
import { trackEvent } from '../analytics'
import { registerTrial } from '../services/publicApi'

export function RegisterPage() {
  const [status, setStatus] = useState<'idle' | 'loading' | 'ok' | 'err'>('idle')
  const [error, setError] = useState('')
  const [subdomain, setSubdomain] = useState('')
  const [trialEnd, setTrialEnd] = useState('')

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setStatus('loading')
    setError('')

    const form = new FormData(event.currentTarget)
    const payload = {
      clinicName: String(form.get('clinicName') || ''),
      contactName: String(form.get('contactName') || ''),
      email: String(form.get('email') || ''),
      phone: String(form.get('phone') || ''),
      subdomain: String(form.get('subdomain') || ''),
      city: String(form.get('city') || ''),
      address: String(form.get('address') || ''),
      consent: form.get('consent') === 'on',
    }

    if (!payload.consent) {
      setStatus('err')
      setError('Please accept the terms to continue.')
      return
    }

    const result = await registerTrial(payload)
    if (!result.success || !result.data) {
      setStatus('err')
      setError(result.errors?.[0] || 'Unable to register right now.')
      trackEvent('register_error')
      return
    }

    setSubdomain(result.data.subdomain)
    setTrialEnd(result.data.trialEndDate || '')
    setStatus('ok')
    trackEvent('register_success', { subdomain: result.data.subdomain })
  }

  return (
    <>
      <Seo
        title="Start free trial"
        description="Register your homoeopathy clinic for a 30-day HomoeoDesk trial."
        path="/register"
      />
      <section className="page-hero">
        <div className="container">
          <h1>Start your free trial</h1>
          <p>
            Create your organization registry entry for a 30-day trial. Our team completes environment
            provisioning and shares clinic access details by email. Meanwhile you can explore the{' '}
            <a href={DEMO_TENANT_URL} target="_blank" rel="noreferrer">
              shared demo
            </a>
            .
          </p>
        </div>
      </section>
      <section className="container form-panel panel">
        {status === 'ok' ? (
          <div className="form-status ok">
            <strong>You are registered.</strong>
            <p style={{ marginTop: '0.5rem' }}>
              Organization subdomain: <code>{subdomain}</code>
              {trialEnd ? <> · Trial through {new Date(trialEnd).toLocaleDateString()}</> : null}
            </p>
            <p style={{ marginTop: '0.5rem' }}>
              We will email next steps for provisioning. Prefer a guided onboarding?{' '}
              <Link to="/request-trial">Leave a note for our team</Link>.
            </p>
          </div>
        ) : (
          <>
            {status === 'err' ? <div className="form-status err">{error}</div> : null}
            <form className="form" onSubmit={onSubmit}>
              <div className="form-row">
                <div className="field">
                  <label htmlFor="clinicName">Clinic / organization name</label>
                  <input id="clinicName" name="clinicName" required />
                </div>
                <div className="field">
                  <label htmlFor="contactName">Your name</label>
                  <input id="contactName" name="contactName" required autoComplete="name" />
                </div>
              </div>
              <div className="form-row">
                <div className="field">
                  <label htmlFor="email">Work email</label>
                  <input id="email" name="email" type="email" required autoComplete="email" />
                </div>
                <div className="field">
                  <label htmlFor="phone">Phone / WhatsApp</label>
                  <input id="phone" name="phone" autoComplete="tel" />
                </div>
              </div>
              <div className="form-row">
                <div className="field">
                  <label htmlFor="subdomain">Preferred subdomain (optional)</label>
                  <input id="subdomain" name="subdomain" placeholder="e.g. greenleaf" pattern="[a-z0-9-]{3,40}" />
                </div>
                <div className="field">
                  <label htmlFor="city">City</label>
                  <input id="city" name="city" />
                </div>
              </div>
              <div className="field">
                <label htmlFor="address">Clinic address (optional)</label>
                <textarea id="address" name="address" />
              </div>
              <label className="checkbox">
                <input type="checkbox" name="consent" />
                <span>
                  I agree to the <Link to="/terms">Terms</Link> and <Link to="/privacy">Privacy Policy</Link>.
                </span>
              </label>
              <button className="button primary" type="submit" disabled={status === 'loading'}>
                {status === 'loading' ? 'Creating trial…' : 'Create trial organization'}
              </button>
            </form>
          </>
        )}
      </section>
    </>
  )
}
