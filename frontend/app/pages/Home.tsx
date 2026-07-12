import React, { useEffect, useState } from 'react';
import { StyleSheet, View, Text } from 'react-native';
import ExpensesSection from '../components/ExpensesSection';
import TemplatesSection from '../components/templatesSection/TemplatesSection';
import { Template } from '../types/Template';
import { useApi } from '../context/ApiContext';

export default function Home() {
    const api = useApi();
    const spreadsheetId = process.env.EXPO_PUBLIC_SPREADSHEET_ID

    const [templates, setTemplates] = useState<Template[]>([])
    const [loading, setLoading] = useState(true);

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
    }, [api]);

    return (
        <View style={styles.home}>
            <ExpensesSection totalExpenses={300}/>
            {loading ? <Text>Loading templates...</Text> : <TemplatesSection templates={templates}/>}
        </View>
    )
}

const styles = StyleSheet.create({
    home: {
        paddingHorizontal: 20,
        gap: 20
    }
})