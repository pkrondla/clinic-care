import { useState } from 'react'
import { Link, NavLink } from 'react-router-dom'
import { navLinks } from '../content/site'
import { trackEvent } from '../analytics'

export function Header() {
  const [open, setOpen] = useState(false)

  return (
    <header className={`site-header${open ? ' open' : ''}`}>
      <div className="container">
        <Link to="/" className="brand-mark" onClick={() => setOpen(false)}>
          Homoeo<span>Desk</span>
        </Link>

        <button
          type="button"
          className="menu-toggle"
          aria-expanded={open}
          aria-label="Toggle navigation"
          onClick={() => setOpen((v) => !v)}
        >
          Menu
        </button>

        <nav className="nav" aria-label="Primary">
          {navLinks.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              className={({ isActive }) => (isActive ? 'active' : undefined)}
              onClick={() => setOpen(false)}
            >
              {link.label}
            </NavLink>
          ))}
        </nav>

        <div className="nav-actions">
          <Link
            to="/demo"
            className="button ghost"
            onClick={() => {
              setOpen(false)
              trackEvent('nav_demo_click')
            }}
          >
            Demo
          </Link>
          <Link
            to="/register"
            className="button primary"
            onClick={() => {
              setOpen(false)
              trackEvent('nav_trial_click')
            }}
          >
            Start trial
          </Link>
        </div>
      </div>
    </header>
  )
}
