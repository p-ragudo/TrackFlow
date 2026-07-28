import { StyleSheet } from "react-native";
import { ApiProvider } from "./context/ApiContext";
import Home from "./pages/Home";
import { SafeAreaView } from "react-native-safe-area-context";
import { ButtonProvider } from "./components/Templates/ButtonProvider";

export default function Index() {
  const apiUrl = process.env.EXPO_PUBLIC_API_URL;
  const apiKey = process.env.EXPO_PUBLIC_API_KEY;

  return (
    <ApiProvider baseUrl={`${apiUrl}`} apiKey={`${apiKey}`} >
      <SafeAreaView style={styles.root}>
        <ButtonProvider>
          <Home />
        </ButtonProvider>
      </SafeAreaView>
    </ApiProvider>
  );
}

const styles = StyleSheet.create({
  root: {
    flex: 1,
  },
});
