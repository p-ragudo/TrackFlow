import React from 'react';
import { StyleSheet, Text, View, Button, StyleProp, ViewStyle } from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';

interface ExpensesSectionProps {
    totalExpenses: number
    totalSavings?: number
    style?: StyleProp<ViewStyle>
}

export default function ExpensesSection({ totalExpenses, totalSavings, style }: ExpensesSectionProps) {
    return (
        <LinearGradient 
            style={[styles.section, style]}
            colors={['#96FFA9', '#0BE39E']}
            start={{ x: 0, y: 0 }}
            end={{ x: 0, y: 1 }}
            locations={[0.4, 1.0]}
        >
            <View>
                <View style={styles.expensesTextSection}>
                    <Text style={styles.expensesText}>
                        Today's Total
                    </Text>
                    <Text style={styles.arrowButton}>
                        ...
                    </Text>
                </View>
                <Text style={styles.cashText}>
                    ₱{totalExpenses.toFixed(2)}
                </Text>
            </View>

            {/* <View>
                <Text style={styles.expensesText}>
                    Today's Savings
                </Text>
                <Text style={styles.cashText}>
                    ₱{totalSavings ? totalExpenses.toFixed(2) : '0.00'}
                </Text>
            </View> */}
        </LinearGradient>
    )
}

const styles = StyleSheet.create({
    section: {
        padding: 26,
        backgroundColor: '#8de28d',
        borderRadius: 14,
        height: 130
    },
    expensesTextSection: {
        flexDirection: "row",
        width: "100%",
        justifyContent: "space-between"
    },
    expensesText: {
        fontSize: 14,
        fontWeight: 600
    },
    arrowButton: {
        fontSize: 16,
    },
    cashText: {
        fontSize: 50,
        fontWeight: "500",
        lineHeight: 44
    }
})