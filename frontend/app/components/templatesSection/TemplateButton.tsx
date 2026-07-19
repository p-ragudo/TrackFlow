import React from 'react';
import { StyleSheet, Text, View, Pressable, DeviceEventEmitter } from 'react-native';
import { Template } from '@/app/types/Template';
import { useApi } from '@/app/context/ApiContext';
import { useGlobalButtons } from './ButtonProvider';

interface TemplateContainerProps {
    template: Template,
}

interface ExpensesPayload {
    name: string,
    group: string,
    category: string,
    tag: string,
    amount: number,
    description: string | null
}

export default function TemplateButton({template}: TemplateContainerProps) {
    const api = useApi()
    const spreadsheetId = process.env.EXPO_PUBLIC_SPREADSHEET_ID

    const { isAnyButtonBusy, triggerAction } = useGlobalButtons();

    const handlePress = async () => {
        triggerAction(async () => {
            try {
                const payload: ExpensesPayload = {
                    name: template.name,
                    group: template.group,
                    category: template.category,
                    tag: template.tag,
                    amount: template.amount,
                    description: null
                }

                const response: any = await api.post(
                    `/api/v1/expenses?spreadsheetid=${spreadsheetId}&sheet=expenses`,
                    payload
                )

                console.log("server response: ", response)

                DeviceEventEmitter.emit('expenseAdded');
            } catch (error: any) {
                console.error("Failed to add expense: ", error);

                if (error.response) {
                    console.log("Error data:", error.response.data);
                    console.log("Error status:", error.response.status);
                }
            }
        })
    }

    return (
        <Pressable
            onPress={handlePress}
            disabled={isAnyButtonBusy}
            style={({pressed}) => [
                {
                    transform: [{ scale: pressed && isAnyButtonBusy ? 0.98 : 1 }],
                    opacity: isAnyButtonBusy ? 0.5 : 1
                },
                styles.section
            ]}
        >
            <Text style={styles.groupText}>
                {template.group}
            </Text>

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
            <Text style={styles.tagText}>
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
    groupText: {
        fontWeight: 800,
        fontSize: 12,
        color: 'gray'
    },
    categoryText: {
        fontWeight: 500,
        fontSize: 12,
        color: 'gray'
    },
    tagText: {
        fontWeight: 500,
        fontSize: 12,
        color: 'gray'
    }
})