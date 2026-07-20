import React from 'react';
import { StyleSheet, Text, View, Button } from 'react-native';
import { Template } from '@/app/types/Template';
import TemplateButton from './TemplateButton';
import { ButtonProvider } from './ButtonProvider';

interface TemplatesSectionProps {
    templates: Template[]
}

export default function TemplatesSection({ templates }: TemplatesSectionProps) {
    return (
        <ButtonProvider>
            <View style={styles.section}>
                {templates.map(template => (
                    <TemplateButton 
                        key={template.id}
                        template={template}
                    />
                ))}
            </View>
        </ButtonProvider>
    )
}

const styles = StyleSheet.create({
    section: {
        gap: 14
    }
})