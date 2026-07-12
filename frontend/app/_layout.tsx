import { Stack } from "expo-router";

export default function RootLayout() {
  return <Stack>
      <Stack.Screen 
        name="index"
        options={{
          title: "",
          headerShadowVisible: false,
          headerStyle: {
            backgroundColor: '#f3f4f6'
          }
        }}
      />
    </Stack>;
}
