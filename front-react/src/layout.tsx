import { useConversations } from './hooks/use-conversations'

function Layout() {
  const {
    data: conversations = [],
    isLoading,
    error,
  } = useConversations()

  if (isLoading) return <div className="container"><p>Loading conversations...</p></div>
  if (error) return <div className="container"><p style={{ color: 'red' }}>Error: {error.message}</p></div>

  return (
    <div className="container">
      <h1>Conversations</h1>
      {conversations.length === 0 ? (
        <p>No conversations found</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Title</th>
              <th>Gizmo ID</th>
              <th>Created</th>
              <th>Updated</th>
            </tr>
          </thead>
          <tbody>
            {conversations.map((conv) => (
              <tr key={conv.id}>
                <td><a href={`/conversations/${conv.id}/html`}>{conv.title}</a></td>
                <td>{conv.gizmoId ?? '-'}</td>
                <td>{new Date(conv.created).toLocaleString()}</td>
                <td>{new Date(conv.updated).toLocaleString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}

export default Layout
