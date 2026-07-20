import { Pressable, StyleSheet, Text } from "react-native";

export default function AddButton() {
    return (
        <Pressable style={styles.container}>
            <Text style={styles.plusText}>
                +
            </Text>
        </Pressable>
    )
}

const styles = StyleSheet.create({
    container: {
        backgroundColor: 'black',
        width: 50,
        height: 50,
        borderRadius: 100
    },
    plusText: {

    }
})