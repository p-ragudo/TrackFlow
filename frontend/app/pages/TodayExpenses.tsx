import { View, Pressable, Text, StyleSheet, ScrollView, RefreshControl } from "react-native"
import { Expense } from "../types/Expense"
import ExpenseView from "../components/ExpenseView"
import { useCallback, useState } from "react"
import Animated, { useAnimatedStyle, withTiming } from 'react-native-reanimated';
import LoadingOverlay from "../components/LoadingOverlay";

interface TodayExpensesProps {
    total: number | string
    onBackButtonPress: () => void
    expenses: Expense[]
    fetchTodayExpenses: () => void
    fetchTodayTotal: () => void
}

export default function TodayExpenses({ total, onBackButtonPress, expenses, fetchTodayExpenses, fetchTodayTotal}: TodayExpensesProps) {
    const [refreshing, setRefreshing] = useState(false);
    const [isSaving, setIsSaving] = useState(false)
    
    const onRefresh = useCallback(async () => {
        setRefreshing(true);
        setIsSaving(true)

        await Promise.all([
            fetchTodayExpenses(),
            fetchTodayTotal()
        ])
        
        setRefreshing(false);
        setIsSaving(false)
    }, []);

    const backdropAnimatedStyle = useAnimatedStyle(() => ({
        opacity: withTiming(isSaving ? 1 : 0, { duration: 200 }),
        pointerEvents: isSaving ? ('auto' as const) : ('none' as const),
    }));

    return (
        <View style={styles.page}>
            <ScrollView
                style={styles.scrollContainer}
                refreshControl={
                    <RefreshControl 
                        refreshing={refreshing} 
                        onRefresh={onRefresh} 
                        tintColor="#000000" // Spinner color on iOS
                        colors={['#000000']} // Spinner color on Android
                    />
                }
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

            <Animated.View style={[styles.backdrop, backdropAnimatedStyle]}>
                <Pressable style={StyleSheet.absoluteFill} />
                <View style={styles.loadingContainer}>
                    <LoadingOverlay text='Loading...' />
                </View>
            </Animated.View>
        </View>
    )
}

const styles = StyleSheet.create({
    page: {
        flex: 1
    },
    scrollContainer: {
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
        fontSize: 18
    },
    totalCashText: {
        fontWeight: 600,
        fontSize: 24
    },
    expensesContainer: {
        gap: 14,
        paddingBottom: 100
    },
    backdrop: {
        ...StyleSheet.absoluteFillObject,
        backgroundColor: 'rgba(0, 0, 0, 0.50)',
        zIndex: 10,
        justifyContent: 'center',
        alignItems: 'center'
    },
    loadingContainer: {
        justifyContent: 'center',
        alignItems: 'center',
        width: '100%',
        paddingHorizontal: 20
    }
})