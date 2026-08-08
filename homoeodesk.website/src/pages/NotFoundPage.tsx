import { Link } from 'react-router-dom'
import { Seo } from '../components/Seo'

export function NotFoundPage() {
  return (
    <>
      <Seo title="Page not found" description="The page you requested was not found." path="/404" />
      <section className="page-hero">
        <div className="container">
          <h1>Page not found</h1>
          <p>That URL is not part of the HomoeoDesk site.</p>
          <div className="hero-actions" style={{ marginTop: '1.25rem' }}>
            <Link to="/" className="button primary">
              Back home
            </Link>
            <Link to="/contact" className="button secondary">
              Contact
            </Link>
          </div>
        </div>
      </section>
    </>
  )
}
