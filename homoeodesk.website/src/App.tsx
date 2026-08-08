import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { Layout } from './components/Layout'
import { HomePage } from './pages/HomePage'
import { FeaturesPage } from './pages/FeaturesPage'
import { DemoPage } from './pages/DemoPage'
import { PricingPage } from './pages/PricingPage'
import { RequestTrialPage } from './pages/RequestTrialPage'
import { RegisterPage } from './pages/RegisterPage'
import { ContactPage } from './pages/ContactPage'
import { AboutPage } from './pages/AboutPage'
import { SecurityPage } from './pages/SecurityPage'
import { PrivacyPage } from './pages/PrivacyPage'
import { TermsPage } from './pages/TermsPage'
import { NotFoundPage } from './pages/NotFoundPage'

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route index element={<HomePage />} />
          <Route path="features" element={<FeaturesPage />} />
          <Route path="demo" element={<DemoPage />} />
          <Route path="pricing" element={<PricingPage />} />
          <Route path="request-trial" element={<RequestTrialPage />} />
          <Route path="register" element={<RegisterPage />} />
          <Route path="subscribe" element={<Navigate to="/register" replace />} />
          <Route path="contact" element={<ContactPage />} />
          <Route path="about" element={<AboutPage />} />
          <Route path="security" element={<SecurityPage />} />
          <Route path="privacy" element={<PrivacyPage />} />
          <Route path="terms" element={<TermsPage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
