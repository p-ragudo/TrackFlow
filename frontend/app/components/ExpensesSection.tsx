import React from 'react';
import { StyleSheet, Text, View, Button } from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';

interface ExpensesSectionProps {
    totalExpenses: number
}

export default function ExpensesSection({ totalExpenses }: ExpensesSectionProps) {
    return (
        <LinearGradient 
            style={styles.section}
            colors={['#96FFA9', '#0BE39E']}
            start={{ x: 0, y: 0 }}
            end={{ x: 0, y: 1 }}
            locations={[0.4, 1.0]}
        >
            <View style={styles.expensesTextSection}>
                <Text style={styles.expensesText}>
                    Today's Total
                </Text>
                <Text style={styles.arrowButton}>
                    ...
                </Text>
            </View>
            <Text style={styles.totalExpensesText}>
                ₱{totalExpenses.toFixed(2)}
            </Text>
        </LinearGradient>
    )
}

const styles = StyleSheet.create({
    section: {
        padding: 26,
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
        fontSize: 14,
        fontWeight: 600
    },
    arrowButton: {
        fontSize: 16,
    },
    totalExpensesText: {
        fontSize: 50,
        fontWeight: "500"
    }
})