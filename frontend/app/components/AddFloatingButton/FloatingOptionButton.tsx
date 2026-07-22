import { Pressable, StyleSheet, Text } from "react-native";

interface FloatingOptionButtonProps {
    text: string
    onPress: () => void
}

export default function FloatingOptionButton({ text, onPress }: FloatingOptionButtonProps) {
    return (
        <Pressable
            style={styles.container}
            onPress={onPress}
        >
            <Text style={styles.text}>
                {text}
            </Text>
        </Pressable>
    )
}

const styles = StyleSheet.create({
    container: {
        backgroundColor: '#FFFFFF',
        paddingVertical: 14,
        paddingHorizontal: 26,
        borderRadius: 25,
        alignItems: 'center',
        justifyContent: 'center',
        marginVertical: 4,
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 2 },
        shadowOpacity: 0.15,
        shadowRadius: 4,
        elevation: 3
    },
    text: {
        fontSize: 16,
        fontWeight: 500
    }
})