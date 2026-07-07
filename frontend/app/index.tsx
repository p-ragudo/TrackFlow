import { Button } from "@react-navigation/elements";
import { Text, View } from "react-native";

export default function Index() {
  return (
    <View
      style={{
        flex: 1,
        justifyContent: "center",
        alignItems: "center",
      }}
    >
      <Text>Let's fucking go bro. Let's goooo. let's fucking gooooo</Text>
      <Button onPressIn={() => console.log("heheehe")}>
        Hehe
      </Button>
    </View>
  );
}
