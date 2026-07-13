import React from 'react';
import { StyleSheet, Text, View, Button } from 'react-native';
import { Template } from '@/app/types/Template';
import TemplateContainer from './TemplateContainer';

interface TemplatesSectionProps {
    templates: Template[]
}

export default function TemplatesSection({ templates }: TemplatesSectionProps) {
    return (
        <View style={styles.section}>
            {templates.map(template => (
                <TemplateContainer 
                    key={template.id}
                    template={template}
                />
            ))}
        </View>
    )
}

const styles = StyleSheet.create({
    section: {
        gap: 14
    }
})