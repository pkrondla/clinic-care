export function ProductVisual() {
  return (
    <div className="product-stage" aria-hidden="true">
      <div className="product-chrome">
        <i />
        <i />
        <i />
      </div>
      <div className="product-grid">
        <div className="product-pane">
          <strong>Queue</strong>
          <ul>
            <li>Riya · waiting</li>
            <li>Arjun · with doctor</li>
            <li>Meera · follow-up</li>
          </ul>
        </div>
        <div className="product-pane">
          <strong>Consultation</strong>
          <ul>
            <li>Complaint & symptoms</li>
            <li>Rx · Pulsatilla 30 · globules</li>
            <li>Follow-up in 7 days</li>
          </ul>
        </div>
        <div className="product-pane">
          <strong>Today</strong>
          <ul>
            <li>12 appointments</li>
            <li>Stock alerts · 3</li>
            <li>Collections on track</li>
          </ul>
        </div>
      </div>
    </div>
  )
}
