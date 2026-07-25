import { StyleSheet, View, Text } from "react-native";

interface LoadingOverlayProps {
    text: string
}

export default function LoadingOverlay({ text }: LoadingOverlayProps) {
    return (
        <View style={styles.container}>
            <Text style={styles.text}>{text}</Text>
        </View>
    )
}

const styles = StyleSheet.create({
    container: {
        paddingHorizontal: 20,
        paddingVertical: 30,
        backgroundColor: 'white',

        borderRadius: 8,
        borderWidth: 0.1,
        borderColor: 'gray',

        justifyContent: 'center',
        alignItems: 'center',

        width: '100%'
    },
    text: {
        fontSize: 14
    }
})