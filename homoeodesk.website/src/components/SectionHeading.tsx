type SectionHeadingProps = {
  title: string
  lead?: string
}

export function SectionHeading({ title, lead }: SectionHeadingProps) {
  return (
    <div className="section-heading">
      <h2>{title}</h2>
      {lead ? <p>{lead}</p> : null}
    </div>
  )
}
