import React from 'react';
import { StyleSheet, Text, View, Pressable } from 'react-native';
import { Template } from '@/app/types/Template';
import { useApi } from '@/app/context/ApiContext';

interface TemplateContainerProps {
    template: Template,
}

interface ExpensesPayload {
    category: string,
    tag: string,
    amount: number,
    description: string | null
}

export default function TemplateButton({template}: TemplateContainerProps) {
    const api = useApi()
    const spreadsheetId = process.env.EXPO_PUBLIC_SPREADSHEET_ID

    const handlePress = async () => {
        try {
            const payload: ExpensesPayload = {
                category: template.category,
                tag: template.tag,
                amount: template.amount,
                description: null
            }

            const response: any = await api.post(
                `/api/expenses?spreadsheetid=${spreadsheetId}&sheet=2026`,
                payload
            )

            console.log("server response: ", response)
        } catch (error) {
            console.error("Failed to add expense using template: ", error)
        }
    }

    return (
        <Pressable
            onPress={handlePress}
            style={({pressed}) => [
                {
                    transform: [{ scale: pressed ? 0.98 : 1 }]
                },
                styles.section
            ]}
        >
            <View style={styles.topRow}>
                <Text style={styles.topRowText}>
                    {template.name}
                </Text>
                <Text style={styles.topRowText}>
                    ₱{template.amount}
                </Text>
            </View>

            <Text style={styles.categoryText}>
                {template.category}
            </Text>
            <Text style={styles.categoryText}>
                {template.tag}
            </Text>
        </Pressable>
    )
}

const styles = StyleSheet.create({
    section: {
        paddingHorizontal: 20,
        paddingVertical: 16,
        backgroundColor: 'white',
        borderRadius: 20
    },
    topRow: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        marginBottom: 4
    },
    topRowText: {
        fontWeight: 700,
        fontSize: 18
    },
    categoryText: {
        fontWeight: 500,
        fontSize: 12,
        color: 'gray'
    }
})