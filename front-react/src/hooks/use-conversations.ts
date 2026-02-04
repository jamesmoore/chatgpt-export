import { useQuery } from '@tanstack/react-query'
import { getConversations, type ConversationSummary } from '../api-client'

export function useConversations() {
  return useQuery<ConversationSummary[], Error>({
    queryKey: ['conversations'],
    queryFn: getConversations,
    select: (data) =>
      [...data].sort(
        (a, b) =>
          new Date(b.created).getTime() - new Date(a.created).getTime(),
      ),
  })
}
