import { useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { useConversations } from './hooks/use-conversations'

export function DocumentTitle() {
  const { id } = useParams()
  const { data: conversations } = useConversations()
  const defaultTitle = 'ChatGPT Archive';
  
  useEffect(() => {
    if (!id || !conversations) {
      document.title = defaultTitle;
      return
    }

    const conversation = conversations.find((c) => c.id === id)
    if (conversation) {
      document.title = conversation.title
    } else {
      document.title = defaultTitle;
    }
  }, [id, conversations])

  return null
}
