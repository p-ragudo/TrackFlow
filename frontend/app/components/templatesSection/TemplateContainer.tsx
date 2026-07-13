import React from 'react';
import { StyleSheet, Text, View, Button } from 'react-native';
import { Template } from '@/app/types/Template';

interface TemplateContainerProps {
    template: Template
}

export default function TemplateContainer({template}: TemplateContainerProps) {
    return (
        <View style={styles.section}>
            <View style={styles.topRow}>
                <Text style={styles.topRowText}>
                    {template.name}
                </Text>
                <Text style={styles.topRowText}>
                    ₱{template.amount}
                </Text>
            </View>

            <Text style={styles.categoryText}>
                {template.category}
            </Text>
            <Text style={styles.categoryText}>
                {template.tag}
            </Text>
        </View>
    )
}

const styles = StyleSheet.create({
    section: {
        paddingHorizontal: 20,
        paddingVertical: 16,
        backgroundColor: 'white',
        borderRadius: 20
    },
    topRow: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        marginBottom: 4
    },
    topRowText: {
        fontWeight: 700,
        fontSize: 18
    },
    categoryText: {
        fontWeight: 500,
        fontSize: 12,
        color: 'gray'
    }
})