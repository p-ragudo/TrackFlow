import React, { useEffect, useState } from 'react';
import { StyleSheet, View, Text } from 'react-native';
import ExpensesSection from '../components/ExpensesSection';
import TemplatesSection from '../components/templatesSection/TemplatesSection';
import { Template } from '../types/Template';
import { useApi } from '../context/ApiContext';
import TabSelector from '../components/TabSelector';

export default function Home() {
    const api = useApi();
    const spreadsheetId = process.env.EXPO_PUBLIC_SPREADSHEET_ID

    const user = 'User';
    const [templates, setTemplates] = useState<Template[]>([])
    const [loading, setLoading] = useState(true);

    const [activeTab, setActiveTab] = useState<'expenses' | 'savings'>('expenses')


    useEffect(() => {
        const fetchTemplates = async () => {
            try {
                setLoading(true);

                const data = await api.get<Template[]>(`/api/templates?spreadsheetid=${spreadsheetId}&sheet=templates`);
                setTemplates(data);
            } catch (error) {
                console.error("Error fetching templates:", error);
            } finally {
                setLoading(false);
            }
        }

        fetchTemplates();
    }, []);

    return (
        <View style={styles.home}>
            <Text style={styles.helloText}>
                Hello, {user}
            </Text>

            <ExpensesSection totalExpenses={300}/>

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
    )
}

const styles = StyleSheet.create({
    home: {
        paddingHorizontal: 20,
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