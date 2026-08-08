import { FormEvent, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { Seo } from '../components/Seo'
import { BOOKING_URL } from '../config'
import { trackEvent } from '../analytics'
import { submitTrialRequest } from '../services/publicApi'

export function RequestTrialPage() {
  const [params] = useSearchParams()
  const plan = params.get('plan') || ''
  const [status, setStatus] = useState<'idle' | 'loading' | 'ok' | 'err'>('idle')
  const [error, setError] = useState('')

  const defaultMessage = useMemo(
    () => (plan ? `Interested in the ${plan} plan.` : ''),
    [plan],
  )

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setStatus('loading')
    setError('')

    const form = new FormData(event.currentTarget)
    const payload = {
      fullName: String(form.get('fullName') || ''),
      email: String(form.get('email') || ''),
      phone: String(form.get('phone') || ''),
      clinicName: String(form.get('clinicName') || ''),
      city: String(form.get('city') || ''),
      patientsPerWeek: String(form.get('patientsPerWeek') || ''),
      message: String(form.get('message') || ''),
      source: plan ? `website:${plan}` : 'website',
      consent: form.get('consent') === 'on',
    }

    if (!payload.consent) {
      setStatus('err')
      setError('Please accept the privacy notice to continue.')
      return
    }

    const result = await submitTrialRequest(payload)
    if (!result.success) {
      setStatus('err')
      setError(result.errors?.[0] || 'Unable to submit right now.')
      trackEvent('trial_request_error')
      return
    }

    setStatus('ok')
    trackEvent('trial_request_submit', { plan: plan || 'none' })
    event.currentTarget.reset()
  }

  return (
    <>
      <Seo
        title="Request a trial"
        description="Request a HomoeoDesk trial for your homoeopathy clinic. Our team will follow up to activate your organization."
        path="/request-trial"
      />
      <section className="page-hero">
        <div className="container">
          <h1>Request a trial</h1>
          <p>
            Prefer a guided start? Tell us about your clinic and we will reach out to activate a trial
            and help with onboarding.
            {BOOKING_URL ? (
              <>
                {' '}
                Or <a href={BOOKING_URL} target="_blank" rel="noreferrer">book a call</a>.
              </>
            ) : null}
          </p>
        </div>
      </section>
      <section className="container form-panel panel">
        {status === 'ok' ? (
          <div className="form-status ok">
            Thanks — your request is in. We typically respond within one business day.
          </div>
        ) : null}
        {status === 'err' ? <div className="form-status err">{error}</div> : null}
        <form className="form" onSubmit={onSubmit}>
          <div className="form-row">
            <div className="field">
              <label htmlFor="fullName">Your name</label>
              <input id="fullName" name="fullName" required autoComplete="name" />
            </div>
            <div className="field">
              <label htmlFor="email">Work email</label>
              <input id="email" name="email" type="email" required autoComplete="email" />
            </div>
          </div>
          <div className="form-row">
            <div className="field">
              <label htmlFor="phone">Phone / WhatsApp</label>
              <input id="phone" name="phone" autoComplete="tel" />
            </div>
            <div className="field">
              <label htmlFor="clinicName">Clinic name</label>
              <input id="clinicName" name="clinicName" required />
            </div>
          </div>
          <div className="form-row">
            <div className="field">
              <label htmlFor="city">City</label>
              <input id="city" name="city" />
            </div>
            <div className="field">
              <label htmlFor="patientsPerWeek">Approx. patients / week</label>
              <select id="patientsPerWeek" name="patientsPerWeek" defaultValue="">
                <option value="" disabled>
                  Select…
                </option>
                <option value="1-25">1–25</option>
                <option value="26-75">26–75</option>
                <option value="76-150">76–150</option>
                <option value="150+">150+</option>
              </select>
            </div>
          </div>
          <div className="field">
            <label htmlFor="message">Anything we should know?</label>
            <textarea id="message" name="message" defaultValue={defaultMessage} />
          </div>
          <label className="checkbox">
            <input type="checkbox" name="consent" />
            <span>I agree to be contacted about HomoeoDesk and accept the Privacy Policy.</span>
          </label>
          <button className="button primary" type="submit" disabled={status === 'loading'}>
            {status === 'loading' ? 'Sending…' : 'Submit request'}
          </button>
        </form>
      </section>
    </>
  )
}
