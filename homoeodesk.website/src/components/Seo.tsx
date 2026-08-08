import { useEffect } from 'react'
import { SITE_NAME, SITE_URL } from '../config'

type SeoProps = {
  title: string
  description: string
  path?: string
}

export function Seo({ title, description, path = '/' }: SeoProps) {
  useEffect(() => {
    const fullTitle = title.includes(SITE_NAME) ? title : `${title} · ${SITE_NAME}`
    document.title = fullTitle

    const setMeta = (selector: string, attr: string, value: string) => {
      let el = document.head.querySelector(selector) as HTMLMetaElement | null
      if (!el) {
        el = document.createElement('meta')
        if (selector.includes('property=')) {
          el.setAttribute('property', selector.match(/property="([^"]+)"/)?.[1] || '')
        } else {
          el.setAttribute('name', selector.match(/name="([^"]+)"/)?.[1] || '')
        }
        document.head.appendChild(el)
      }
      el.setAttribute(attr, value)
    }

    setMeta('meta[name="description"]', 'content', description)
    setMeta('meta[property="og:title"]', 'content', fullTitle)
    setMeta('meta[property="og:description"]', 'content', description)
    setMeta('meta[property="og:type"]', 'content', 'website')
    setMeta('meta[property="og:url"]', 'content', `${SITE_URL}${path}`)
    setMeta('meta[property="og:site_name"]', 'content', SITE_NAME)
    setMeta('meta[name="twitter:card"]', 'content', 'summary_large_image')
    setMeta('meta[name="twitter:title"]', 'content', fullTitle)
    setMeta('meta[name="twitter:description"]', 'content', description)

    let canonical = document.head.querySelector('link[rel="canonical"]') as HTMLLinkElement | null
    if (!canonical) {
      canonical = document.createElement('link')
      canonical.rel = 'canonical'
      document.head.appendChild(canonical)
    }
    canonical.href = `${SITE_URL}${path}`
  }, [title, description, path])

  return null
}
