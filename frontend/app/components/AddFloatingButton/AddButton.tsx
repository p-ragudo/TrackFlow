import { Pressable, StyleSheet, Text } from "react-native";

export default function AddButton() {
    return (
        <Pressable 
            style={styles.container}
            onPress={() => console.log("add button pressed")}
        >
            <Text style={styles.plusText}>
                +
            </Text>
        </Pressable>
    )
}

const styles = StyleSheet.create({
    container: {
        position: 'absolute',

        bottom: 20,
        left: '50%',
        marginLeft: -30,

        width: 60,
        height: 60,
        borderRadius: 30,
        backgroundColor: 'black',

        alignItems: 'center',
        justifyContent: 'center',

        elevation: 5,
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 2 },
        shadowOpacity: 0.3,
        shadowRadius: 3,
    },
    plusText: {
        color: 'white',
        fontSize: 28,
        fontWeight: 400,
        lineHeight: 30,
    }
})