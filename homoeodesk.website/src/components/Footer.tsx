import { Link } from 'react-router-dom'
import { CONTACT_EMAIL } from '../config'

export function Footer() {
  const year = new Date().getFullYear()

  return (
    <footer className="site-footer">
      <div className="container">
        <div className="footer-grid">
          <div>
            <div className="footer-brand">HomoeoDesk</div>
            <p style={{ color: 'var(--muted)', maxWidth: '34ch' }}>
              Modern clinic management for homoeopathy practices — care, ops, pharmacy, and billing in one place.
            </p>
          </div>
          <div className="footer-col">
            <h3>Product</h3>
            <Link to="/features">Features</Link>
            <Link to="/pricing">Pricing</Link>
            <Link to="/demo">Demo</Link>
            <Link to="/register">Start trial</Link>
          </div>
          <div className="footer-col">
            <h3>Company</h3>
            <Link to="/about">About</Link>
            <Link to="/security">Security</Link>
            <Link to="/contact">Contact</Link>
            <a href={`mailto:${CONTACT_EMAIL}`}>{CONTACT_EMAIL}</a>
          </div>
          <div className="footer-col">
            <h3>Legal</h3>
            <Link to="/privacy">Privacy</Link>
            <Link to="/terms">Terms</Link>
            <Link to="/request-trial">Request trial</Link>
          </div>
        </div>
        <div className="footer-bottom">
          <span>© {year} HomoeoDesk</span>
          <span>Built for homoeopathy clinics</span>
        </div>
      </div>
    </footer>
  )
}
