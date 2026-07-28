import React from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { Template } from '@/app/types/Template';
import TemplateButton from './TemplateButton';

interface TemplatesSectionProps {
    spreadsheetId: string,
    templates: Template[]
}

export default function TemplatesSection({ spreadsheetId, templates }: TemplatesSectionProps) {
    return (
        <View style={styles.section}>
            {templates.length > 0 ? (
                templates.map(template => (
                    <TemplateButton 
                        key={template.id}
                        spreadsheetId={spreadsheetId}
                        template={template}
                    />
                ))
            ) : (
                <Text>No templates available. Add one now.</Text>
            )}
        </View>
    )
}

const styles = StyleSheet.create({
    section: {
        gap: 14,
        marginBottom: 100
    }
})