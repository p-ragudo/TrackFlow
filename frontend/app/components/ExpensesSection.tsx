import React from 'react';
import { StyleSheet, Text, View, Button } from 'react-native';

interface ExpensesSectionProps {
    totalExpenses: number
}

export default function ExpensesSection({ totalExpenses }: ExpensesSectionProps) {
    return (
        <View style={styles.section}>
            <View style={styles.expensesTextSection}>
                <Text style={styles.expensesText}>
                    Expenses
                </Text>
                <Text style={styles.arrowButton}>
                    {">"}
                </Text>
            </View>
            <Text style={styles.totalExpensesText}>
                {totalExpenses}
            </Text>
        </View>
    )
}

const styles = StyleSheet.create({
    section: {
        padding: 20,
        backgroundColor: '#8de28d',
        borderRadius: 14,
        height: 150,
    },
    expensesTextSection: {
        flexDirection: "row",
        width: "100%",
        justifyContent: "space-between",
    },
    expensesText: {
        fontSize: 16,
        color: 'gray',
        fontWeight: 300
    },
    arrowButton: {
        fontSize: 16,
    },
    totalExpensesText: {
        fontSize: 40,
        fontWeight: "500"
    }
})