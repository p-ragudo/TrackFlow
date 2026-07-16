import React, { useEffect, useState } from 'react';
import { StyleSheet, View, Text, DeviceEventEmitter, ScrollView } from 'react-native';
import ExpensesSection from '../components/ExpensesSection';
import TemplatesSection from '../components/templatesSection/TemplatesSection';
import { Template } from '../types/Template';
import { useApi } from '../context/ApiContext';
import TabSelector from '../components/TabSelector';

export default function Home() {
    const api = useApi();
    const spreadsheetId = process.env.EXPO_PUBLIC_SPREADSHEET_ID

    const [todayTotal, setTodayTotal] = useState(0)
    const user = 'User';
    const [templates, setTemplates] = useState<Template[]>([])
    const [loading, setLoading] = useState(true);

    const [activeTab, setActiveTab] = useState<'expenses' | 'savings'>('expenses')

    const fetchTemplates = async () => {
            try {
                const data = await api.get<Template[]>(`/api/templates?spreadsheetid=${spreadsheetId}&sheet=templates`);
                setTemplates(data);
            } catch (error) {
                throw new Error(`Error fetching templates: ${error}`);
            }
        }

        const fetchTodayTotal = async () => {
            try {
                const data = await api.get<number>(`/api/expenses/today?spreadsheetid=${spreadsheetId}&sheet=Expenses`);
                setTodayTotal(data)
            } catch (error) {
                throw new Error(`Error fetching today's total: ${error}`);
            }
        }

        const fetchData = async () => {
            try {
                setLoading(true);

                await fetchTemplates();
                await fetchTodayTotal();
            } catch (error) {
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
        <ScrollView showsVerticalScrollIndicator={false}>
            <View style={styles.home}>
                <Text style={styles.helloText}>
                    Hello, {user}
                </Text>

                <ExpensesSection totalExpenses={todayTotal}/>

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
    )
}

const styles = StyleSheet.create({
    home: {
        paddingHorizontal: 20,
        paddingBottom: 45,
        gap: 20
    },
    helloText: {
        fontWeight: 'bold',
        fontSize: 30,
        marginBottom: 20
    },
    tabSection: {
        flexDirection: 'row',
        width: '100%',
        gap: 12
    }
})