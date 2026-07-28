import { Stack } from "expo-router";
import { SafeAreaProvider } from "react-native-safe-area-context";
import { TestUserProvider } from "./context/TestUserContext";

export default function RootLayout() {
  return <SafeAreaProvider>
    <TestUserProvider>
      <Stack>
        <Stack.Screen 
          name="index"
          options={{
            headerShown: false
          }}
        />
      </Stack>
    </TestUserProvider>
  </SafeAreaProvider>
}
