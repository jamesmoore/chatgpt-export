import { Spinner } from "./components/ui/spinner";

export default function LoadingSpinner() {
    return <div className="flex flex-1 justify-center items-center delayed-fade-in">
        <Spinner />
    </div>;
}
