
import { View, StyleSheet } from "react-native";
import { ApiProvider } from "./context/ApiContext";
import Home from "./pages/Home";

export default function Index() {
  const apiUrl = process.env.EXPO_PUBLIC_API_URL;

  return (
    <ApiProvider baseUrl={`${apiUrl}`}>
      <View>
        <Home />
      </View>
    </ApiProvider>
  );
}
