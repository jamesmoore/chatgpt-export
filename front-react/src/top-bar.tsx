import { SidebarTrigger } from "./components/ui/sidebar";

export function TopBar() {
  return (
        <div className="flex py-1 pr-1 items-center">
            <SidebarTrigger />
        </div>
  );
}