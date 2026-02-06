import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarSeparator,
  useSidebar,
} from "@/components/ui/sidebar"
import { useEffect } from "react";
import { Link, useLocation, useParams } from "react-router-dom";
import { useConversations } from '../hooks/use-conversations'

export function AppSidebar() {
  const { id, format } = useParams();
  const location = useLocation();
  const { setOpenMobile } = useSidebar();

  const {
    data: conversations = []
  } = useConversations();

  useEffect(() => {
    setOpenMobile(false);
  }, [location.pathname, setOpenMobile]);

  return (
    <Sidebar className="h-full">
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel className="text-base my-0.5">Conversations</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              <SidebarSeparator />
              {conversations.map((item) => (
                <SidebarMenuItem key={item.id}>
                  <SidebarMenuButton asChild isActive={id === item.id}>
                    <Link to={`/conversation/${item.id}/${format || 'html'}`}
                      onClick={() => {
                        if (id === item.id) {
                          setOpenMobile(false);
                        }
                      }}
                    >
                      <span>{item.title}</span>
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
      <SidebarFooter>
      </SidebarFooter >
    </Sidebar>
  )
}

