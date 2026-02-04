import { ChevronRight, LogOut, Mail, MailOpen, Settings, Trash2, User2 } from "lucide-react"

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
import { Fragment, useEffect, useMemo } from "react";
import { Link, useLocation, useParams } from "react-router-dom";

import { useConversations } from '../hooks/use-conversations'

interface SidebarItem {
  key: string,
  title: string,
  url: string,
  selected: boolean,
  hasSeparator: boolean,
  header?: string,
  indent?: boolean,
}

export function AppSidebar() {

  const location = useLocation();
  const { setOpenMobile } = useSidebar();

  const {
    data: conversations = [],
    isLoading,
    error,
  } = useConversations();

  useEffect(() => {
    setOpenMobile(false);
  }, [location.pathname, setOpenMobile]);

  if (isLoading) return <div className="container"><p>Loading conversations...</p></div>
  if (error) return <div className="container"><p style={{ color: 'red' }}>Error: {error.message}</p></div>

  return (
    <Sidebar className="h-full">
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel className="text-base my-0.5">Conversations</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              <SidebarSeparator />
              {conversations.map((item) => (
                <Fragment key={item.id}>
                  <SidebarMenuItem  >
                    <SidebarMenuButton asChild >
                      <Link to={`/conversation/${item.id}/html`}
                        onClick={() => {
                          // if (item.selected) {
                          //   setOpenMobile(false);
                          // }
                        }}
                      >
                        <span>{item.title}</span>
                      </Link>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                </Fragment>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
      <SidebarFooter>
        {/* {user && user.requiresAuth &&
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <SidebarMenuButton className="text-base">
                {user.picture ?
                  <Avatar>
                    <AvatarImage src={user.picture} />
                  </Avatar>
                  : <User2 />
                }
                {user.name ?? user.email}
                <ChevronRight className="ml-auto" />
              </SidebarMenuButton>
            </DropdownMenuTrigger>
            <DropdownMenuContent side="right">
              <DropdownMenuItem className="text-base" onClick={doLogout}>
                <LogOut /><span>Sign out</span>
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        } */}
      </SidebarFooter >
    </Sidebar>
  )
}

