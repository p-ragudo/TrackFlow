import { View, Pressable, Text, StyleSheet, ScrollView } from "react-native"
import { Expense } from "../types/Expense"
import ExpenseView from "../components/ExpenseView"

interface TodayExpensesProps {
    total: number | string
    onBackButtonPress: () => void
    expenses: Expense[]
}

export default function TodayExpenses({ total, onBackButtonPress, expenses}: TodayExpensesProps) {
    return (
        <ScrollView
            style={styles.page}
        >
            <View>
                <View style={styles.header}>
                    <Text style={styles.headerText}>Today's Expenses</Text>
                    <Pressable 
                        style={styles.homeButton}
                        onPress={onBackButtonPress}
                    >
                        <Text style={styles.homeButtonText}>Back</Text>
                    </Pressable>
                </View>

                <View style={styles.totalWrapper}>
                    <Text style={styles.totalText}>Total:</Text>
                    <Text style={styles.totalCashText}>₱{total}</Text>
                </View>

                <View style={styles.expensesContainer}>
                    {expenses.length === 0 ? (
                        <Text>No expenses today yet!</Text>
                    ) : (
                        expenses.map(expense => (
                            <ExpenseView 
                                key={expense.id}
                                expense={expense}
                            />
                        ))
                    )}
                </View>
            </View>
        </ScrollView>
    )
}

const styles = StyleSheet.create({
    page: {
        padding: 20
    },
    header: {
        flexDirection: 'row',
        justifyContent: 'space-between'
    },
    headerText: {
        fontSize: 24,
        fontWeight: 500
    },
    homeButton: {
        backgroundColor: 'black',
        paddingVertical: 8,
        paddingHorizontal: 16,
        borderRadius: 6 
    },
    homeButtonText: {
        color: 'white'
    },
    totalWrapper: {
        flexDirection: 'row',
        flex: 1,
        justifyContent: 'space-between',
        alignItems: 'center',
        marginTop: 30,
        marginBottom: 8
    },
    totalText: {
        fontWeight: 600,
        fontSize: 20
    },
    totalCashText: {
        fontWeight: 600,
        fontSize: 36
    },
    expensesContainer: {
        gap: 14
    },
})