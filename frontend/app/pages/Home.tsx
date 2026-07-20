import React, { useEffect, useState } from 'react';
import { StyleSheet, View, Text, DeviceEventEmitter, ScrollView } from 'react-native';
import ExpensesSection from '../components/ExpensesSection';
import TemplatesSection from '../components/Templates/TemplatesSection';
import { Template } from '../types/Template';
import { useApi } from '../context/ApiContext';
import TabSelector from '../components/TabSelector';
import AddButton from '../components/AddFloatingButton/AddButton';

interface TemplatesResponse {
    templates: Template[]
}

interface TodayTotalResponse {
    total: number
}

export default function Home() {
    const api = useApi();
    const spreadsheetId = process.env.EXPO_PUBLIC_SPREADSHEET_ID

    const [errors, setErrors] = useState<string[]>([])
    const [todayTotal, setTodayTotal] = useState(0)
    const user = 'Paolo';
    const [templates, setTemplates] = useState<Template[]>([])
    const [loading, setLoading] = useState(true);

    const [activeTab, setActiveTab] = useState<'expenses' | 'savings'>('expenses')

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
            } catch (error) {
                const messagePrefix = "Error in fetchData"
                const errorMessage = error instanceof Error ? `${messagePrefix} ${error.message}` : String(error);
                setErrors((prev) => [...prev, errorMessage])
                console.log("Error in method fetchData:", error);
            } finally {
                setLoading(false);
            }
        }

    useEffect(() => {
        fetchData();

        const subscription = DeviceEventEmitter.addListener('expenseAdded', () => {
            fetchTodayTotal();
        })

        return () => subscription.remove();
    }, []);

    return (
        <View style={styles.container}>
            <ScrollView
                showsVerticalScrollIndicator={false}
            >
                <View style={styles.home}>
                    <Text style={styles.helloText}>
                        Hello, {user}
                    </Text>

                    <ExpensesSection 
                        totalExpenses={todayTotal}
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

            <AddButton />
        </View>
    )
}

const styles = StyleSheet.create({
    container: {
        marginTop: 20,
        flex: 1
    },
    home: {
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
    }
})