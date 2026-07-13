import React from 'react';
import { StyleSheet, Text, Pressable } from 'react-native';

interface TabSelectorProps {
    name: string,
    selected: boolean,
    onPress: () => void
}

export default function TabSelector({ name, selected, onPress }: TabSelectorProps) {
    return (
        <Pressable 
            onPress={onPress}
            style={[
                styles.section,
                { backgroundColor: selected ? 'black' : 'white'}
            ]}
        >
            <Text
                style={[
                    styles.text,
                    { color: selected ? 'white' : 'black' }
                ]}
            >
                {name}
            </Text>
        </Pressable>
    )
}

const styles = StyleSheet.create({
    section: {
        flex: 1,
        height: 42,
        justifyContent: 'center',
        alignItems: 'center',
        borderRadius: 100,
    },
    text: {
        fontWeight: 500,
        fontSize: 16
    }
})