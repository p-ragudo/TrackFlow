import React from 'react';
import { StyleSheet, Text, View, Button, StyleProp, ViewStyle, Pressable } from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';

interface ExpensesSectionProps {
    totalExpenses: number
    style?: StyleProp<ViewStyle>
    seeAllButtonOnPress: () => void
}

export default function ExpensesSection({ totalExpenses, style, seeAllButtonOnPress }: ExpensesSectionProps) {
    return (
        <Pressable onPress={seeAllButtonOnPress}>
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
                        <Pressable
                            onPress={seeAllButtonOnPress}
                        >
                            <Text style={styles.seeAllText}>See all</Text>
                        </Pressable>
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
        </Pressable>
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
    seeAllText: {
        fontSize: 12,
        fontWeight: 800,
        color: 'dark-green'
    },
    cashText: {
        fontSize: 50,
        fontWeight: "500",
        lineHeight: 44
    }
})