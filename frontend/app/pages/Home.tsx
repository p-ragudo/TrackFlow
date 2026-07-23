import React, { useCallback, useMemo, useEffect, useState } from 'react';
import { StyleSheet, View, Text, DeviceEventEmitter, ScrollView, Pressable, RefreshControl } from 'react-native';
import Animated, { useAnimatedStyle, withTiming } from 'react-native-reanimated';
import ExpensesSection from '../components/ExpensesSection';
import TemplatesSection from '../components/Templates/TemplatesSection';
import { Template } from '../types/Template';
import { useApi } from '../context/ApiContext';
import TabSelector from '../components/TabSelector';
import AddButton from '../components/AddFloatingButton/AddButton';
import AddPage from './AddPage';

interface TemplatesResponse {
    templates: Template[]
}

interface TodayTotalResponse {
    total: number
}

type Pages = 'home' | 'addExpense' | 'addExpenseTemplate'

export default function Home() {
    const api = useApi();
    const spreadsheetId = process.env.EXPO_PUBLIC_SPREADSHEET_ID

    const [errors, setErrors] = useState<string[]>([])
    const [todayTotal, setTodayTotal] = useState(0)
    const user = 'Paolo';
    const [templates, setTemplates] = useState<Template[]>([])
    const [loading, setLoading] = useState(true);
    const [activeTab, setActiveTab] = useState<'expenses' | 'savings'>('expenses')
    const [isAddMenuOpen, setIsAddMenuOpen] = useState(false);
    const [refreshing, setRefreshing] = useState(false);

    const [activePage, setActivePage] = useState<Pages>('home')

    // Dimming backdrop style
    const backdropAnimatedStyle = useAnimatedStyle(() => ({
        opacity: withTiming(isAddMenuOpen ? 1 : 0, { duration: 200 }),
        pointerEvents: isAddMenuOpen ? ('auto' as const) : ('none' as const),
    }));

    const fetchTemplates = async () => {
        try {
            const response = await api.get<TemplatesResponse>(`/api/v1/templates?spreadsheetid=${spreadsheetId}&sheet=templates`);
            setTemplates(response.templates);
        } catch (error) {
            const messagePrefix = "Error in fetchTemplates"
            const errorMessage = error instanceof Error ? `${messagePrefix} ${error.message}` : String(error);
            setErrors((prev) => [...prev, errorMessage])
            throw new Error(`Error fetching templates: ${error}`);
        }
    }

    const fetchTodayTotal = async () => {
        try {
            const response = await api.get<TodayTotalResponse>(`/api/v1/expenses/today/total?spreadsheetid=${spreadsheetId}&sheet=expenses`);
            setTodayTotal(response.total)
        } catch (error) {
            const messagePrefix = "Error in fetchTodayTotal"
            const errorMessage = error instanceof Error ? `${messagePrefix} ${error.message}` : String(error);
            setErrors((prev) => [...prev, errorMessage])
            throw new Error(`Error fetching today's total: ${error}`);
        }
    }

    const fetchData = async () => {
        try {
            setLoading(true);

            await fetchTemplates();
            await fetchTodayTotal();

            setErrors([])
        } catch (error) {
            const messagePrefix = "Error in fetchData"
            const errorMessage = error instanceof Error ? `${messagePrefix} ${error.message}` : String(error);
            setErrors((prev) => [...prev, errorMessage])
            console.log("Error in method fetchData:", error);
        } finally {
            setLoading(false);
        }
    }

    const groupOptions = useMemo<string[]>(() => {
        return Array.from(new Set(templates.map((item) => item.group)));
    }, [templates])

    const categoryOptions = useMemo<string[]>(() => {
        return Array.from(new Set(templates.map((item) => item.category)));
    }, [templates])

    const tagOptions = useMemo<string[]>(() => {
        return Array.from(new Set(templates.map((item) => item.tag)));
    }, [templates])

    useEffect(() => {
        fetchData();

        const subscription = DeviceEventEmitter.addListener('expenseAdded', () => {
            fetchTodayTotal();
        })

        return () => subscription.remove();
    }, []);

    const onRefresh = useCallback(async () => {
        setRefreshing(true);
        await fetchData();
        setRefreshing(false);
    }, []);

    const handleOptionPressed = (activePage: Pages) => {
        setIsAddMenuOpen(false);
        setActivePage(activePage);
    }

    const renderContent = (activePage: string) => {
        switch (activePage) {
            case 'home':
                return (
                    <View style={styles.container}>
                        <ScrollView
                            showsVerticalScrollIndicator={false}
                            refreshControl={
                                <RefreshControl 
                                    refreshing={refreshing} 
                                    onRefresh={onRefresh} 
                                    tintColor="#000000" // Spinner color on iOS
                                    colors={['#000000']} // Spinner color on Android
                                />
                            }
                        >
                            <View style={styles.mainPage}>
                                <Text style={styles.helloText}>
                                    Hello, {user}
                                </Text>

                                <ExpensesSection 
                                    totalExpenses={loading ? 0.00 : todayTotal}
                                    style={styles.expensesSection}
                                />

                                {errors.map((error, index) => (
                                    <Text key={index} style={styles.errorText}>
                                        {error}
                                    </Text>
                                ))}

                                <View style={styles.tabSection}>
                                    <TabSelector 
                                        name='Expenses' 
                                        selected={activeTab === 'expenses'} 
                                        onPress={() => setActiveTab('expenses')} 
                                    />
                                    <TabSelector 
                                        name='Savings' 
                                        selected={activeTab === 'savings'} 
                                        onPress={() => setActiveTab('savings')} 
                                    />
                                </View>

                                {loading ? <Text>Loading templates...</Text> : <TemplatesSection templates={templates}/>}
                            </View>
                        </ScrollView>

                        {/* Dark Backdrop Overlay */}
                        <Animated.View style={[styles.backdrop, backdropAnimatedStyle]}>
                            <Pressable 
                                style={StyleSheet.absoluteFill} 
                                onPress={() => setIsAddMenuOpen(false)} 
                            />
                        </Animated.View>

                        {/* Floating Action Button */}
                        <AddButton 
                            isOpen={isAddMenuOpen} 
                            onToggle={() => setIsAddMenuOpen((prev) => !prev)}
                            onAddExpensePressed={() => handleOptionPressed('addExpense')} 
                            onAddExpenseTemplatePressed={() => handleOptionPressed('addExpenseTemplate')}
                        />
                    </View>
                )
            case 'addExpense':
                return (
                    <AddPage 
                        title='Add Expense'
                        onCancelPressed={() => handleOptionPressed('home')}
                        onSavePressed={() => handleOptionPressed('home')}
                        groups={groupOptions}
                        categories={categoryOptions}
                        tags={tagOptions}
                    />
                )
            case 'addExpenseTemplate':
                return (
                    <AddPage 
                        title='Add Template'
                        onCancelPressed={() => handleOptionPressed('home')}
                        onSavePressed={() => handleOptionPressed('home')}
                        groups={groupOptions}
                        categories={categoryOptions}
                        tags={tagOptions}
                    />
                )
        }
    }

    return (
        renderContent(activePage)
    )
}

const styles = StyleSheet.create({
    container: {
        flex: 1
    },
    mainPage: {
        marginTop: 20,
        paddingHorizontal: 20,
        gap: 20
    },
    helloText: {
        fontWeight: 'bold',
        fontSize: 24,
        marginBottom: 24
    },
    expensesSection: {
        marginBottom: 28
    },
    tabSection: {
        flexDirection: 'row',
        width: '100%',
        gap: 12
    },
    errorText: {
        color: 'red',
        fontSize: 14,
        marginBottom: 8,
    },
    backdrop: {
        ...StyleSheet.absoluteFillObject,
        backgroundColor: 'rgba(0, 0, 0, 0.50)', // Matching gray dim overlay from reference image
        zIndex: 10,
    }
})